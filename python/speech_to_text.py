import whisper
import pyaudio
import numpy as np
import webrtcvad
import queue
import threading
import logging
import noisereduce as nr  # Import the noise reduction library

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Load Whisper model
model = whisper.load_model("small")

# Initialize PyAudio for capturing audio from the microphone
p = pyaudio.PyAudio()

# Audio stream setup
FORMAT = pyaudio.paInt16  # 16-bit audio format
CHANNELS = 1  # Mono channel
RATE = 16000  # 16 kHz sample rate (Whisper prefers this rate)
CHUNK = 320  # 20 ms chunk size (16000 samples/second * 0.02 seconds)

stream = p.open(format=FORMAT, channels=CHANNELS, rate=RATE, input=True, frames_per_buffer=CHUNK)

# Initialize WebRTC VAD for detecting speech and pauses
vad = webrtcvad.Vad()
vad.set_mode(3)  # Sensitivity level (0 to 3, where 3 is the most sensitive)

# Queue for holding audio data to be transcribed
transcription_queue = queue.Queue()

def transcribe_audio(audio_data):
    # Use Whisper to transcribe the audio data
    audio_data = np.frombuffer(audio_data, dtype=np.int16).astype(np.float32) / 32768.0
    result = model.transcribe(audio_data, language="fr")  # Specify French language
    return result["text"]

def transcription_worker():
    # Worker thread that processes audio data from the queue
    while True:
        audio_buffer = transcription_queue.get()
        if audio_buffer is None:
            break  # Exit the worker when None is received
        transcription = transcribe_audio(audio_buffer)
        logger.info(f"Transcription: {transcription}")
        transcription_queue.task_done()

# Start the transcription thread
thread = threading.Thread(target=transcription_worker, daemon=True)
thread.start()

def calculate_dominant_frequency(audio_buffer, rate):
    """Calculate the dominant frequency in the audio buffer."""
    # Apply FFT on the audio buffer
    audio_fft = np.fft.fft(audio_buffer)
    
    # Calculate frequencies corresponding to FFT bins
    freqs = np.fft.fftfreq(len(audio_fft), 1/rate)
    
    # Only use the positive half of the frequencies
    audio_fft = np.abs(audio_fft[:len(audio_fft)//2])
    freqs = freqs[:len(freqs)//2]
    
    # Find the index of the dominant frequency
    dominant_index = np.argmax(audio_fft)
    
    # Get the dominant frequency
    dominant_frequency = freqs[dominant_index]
    
    return dominant_frequency

def is_human_voice(frequency):
    """Check if the frequency is within the range of human voice."""
    return 85 <= frequency <= 300  # Typical range for human voice fundamentals

def reduce_noise(audio_buffer, rate):
    """Apply noise reduction on the audio buffer."""
    # Reduce noise from the audio buffer
    reduced_noise = nr.reduce_noise(y=audio_buffer.astype(np.float32), sr=rate)
    return reduced_noise.astype(np.int16)

logger.info("Listening for speech... Press Ctrl+C to stop.")
audio_buffer = np.zeros((0), dtype=np.int16)
is_speaking = False

try:
    while True:
        audio_chunk = stream.read(CHUNK)
        audio_data = np.frombuffer(audio_chunk, dtype=np.int16)

        # Apply noise reduction
        audio_data = reduce_noise(audio_data, RATE)

        # Check if the current audio chunk contains speech using VAD
        if vad.is_speech(audio_chunk, RATE):
            is_speaking = True
            audio_buffer = np.append(audio_buffer, audio_data)
            logger.info("Speech detected, collecting audio...")
        else:
            if is_speaking and len(audio_buffer) > 0:
                # If there was speech but now there's a pause, calculate the dominant frequency
                dominant_frequency = calculate_dominant_frequency(audio_buffer, RATE)
                logger.info(f"Dominant frequency: {dominant_frequency:.2f} Hz")
                
                # Check if the dominant frequency is within the human voice range
                if is_human_voice(dominant_frequency):
                    logger.info(f"Human voice detected (Frequency: {dominant_frequency:.2f} Hz), adding to transcription queue...")
                    transcription_queue.put(audio_buffer)
                else:
                    logger.info(f"Non-human voice or noise detected (Frequency: {dominant_frequency:.2f} Hz), ignoring...")
                
                # Clear the buffer and reset speaking flag after processing
                audio_buffer = np.zeros((0), dtype=np.int16)
                is_speaking = False

except KeyboardInterrupt:
    logger.info("Stopping transcription.")
    stream.stop_stream()
    stream.close()
    p.terminate()

    # Stop the transcription thread
    transcription_queue.put(None)
    thread.join()
