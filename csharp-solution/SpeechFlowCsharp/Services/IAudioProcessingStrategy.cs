using System.Collections.Concurrent;
using SpeechFlowCsharp.Domain;

namespace SpeechFlowCsharp.Services
{
    /// <summary>
    /// Interface pour toute stratégie de traitement/découpage de l'audio
    /// (ex: chunk fixe, détection de silence, nombre de mots...).
    /// </summary>
    public interface IAudioProcessingStrategy
    {
        /// <summary>
        /// Méthode appelée lorsqu'on reçoit des données audio (buffer) de longueur byteCount.
        /// La stratégie doit décider comment découper ou accumuler ce buffer
        /// puis créer éventuellement un ou plusieurs AudioChunk à envoyer dans la queue.
        /// </summary>
        /// <param name="buffer">Le buffer audio brut (16 bits, mono)</param>
        /// <param name="byteCount">Taille valable dans le buffer</param>
        /// <param name="chunkQueue">Queue dans laquelle pousser les AudioChunk</param>
        void ProcessAudio(byte[] buffer, int byteCount, BlockingCollection<AudioChunk> chunkQueue);
    }
}
