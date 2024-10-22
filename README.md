# SpeechFlow

**SpeechFlow** est une solution complète de transcription et de traduction en temps réel de la parole. Il combine la puissance de **Python** pour la reconnaissance vocale avec le modèle **Whisper d'OpenAI** et l'efficacité de **C#** pour gérer la traduction et la transformation des résultats en **JSON**.

## Fonctionnalités principales

- **Reconnaissance vocale en temps réel** : Utilise **PyAudio** pour capturer l’audio depuis un microphone. Le modèle **Whisper** transcrit cet audio en texte, optimisé par un système de détection de pauses basé sur **WebRTC VAD** afin de ne transcrire que les portions significatives de la parole.
  
- **Traduction automatique** : Le texte transcrit est envoyé à un serveur **C#** via des **sockets**. Ce serveur utilise l'API **LibreTranslate** pour traduire le texte du **français vers l'anglais**.

- **Transformation en JSON** : Les résultats de la transcription et de la traduction sont formatés en **JSON** avec des métadonnées, telles que l’horodatage. Le JSON est ensuite sauvegardé dans un fichier pour une utilisation future.

- **Intégration Python et C#** : Le projet combine **Python** pour la reconnaissance vocale et **C#** pour la gestion des traductions et des fichiers, offrant une solution intégrée et fluide pour la gestion de la parole et des données textuelles.

## Objectif du projet

**SpeechFlow** est destiné à des applications nécessitant de la **reconnaissance vocale**, de la **traduction** et de la **gestion des données textuelles** en temps réel. Il peut être utilisé pour :
- La transcription automatique de discours multilingues.
- Des outils de traduction vocale en temps réel.
- Des services d'analyse de texte à partir de la parole.

Ce projet met en œuvre des technologies modernes, telles que **Whisper** et **LibreTranslate**, pour offrir une solution robuste et flexible pour la transcription et la traduction de la parole.

## Installation

Ce projet a été optimisé pour la performance avec une faible latence en utilisant un traitement asynchrone de l'audio et peut tirer parti de l'accélération GPU pour une transcription plus rapide.

## Fonctionnalités (Python)
- Transcription audio en temps réel avec Whisper
- Détection d'activité vocale (VAD) avec WebRTC
- Réduction de bruit avec la bibliothèque `noisereduce`
- Traitement audio à faible latence avec I/O asynchrone
- Accélération GPU optionnelle avec PyTorch

## Prérequis (Python)
- Python 3.8+
- PyTorch avec support CUDA (pour l'accélération GPU)
- PyAudio (pour capturer l'audio depuis le microphone)
- Whisper d'OpenAI (pour la transcription)

### Python

1. Installez les dépendances Python en utilisant le fichier `requirements.txt` :
   ```bash
   pip install -r python/requirements.txt

2. Whisper documentation: https://github.com/openai/whisper

3. Cuda and torch: https://pytorch.org/get-started/locally/
  ```bash
  pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

# SpeechFlowCsharp (C#)

**SpeechFlowCsharp** est un projet C# conçu pour capturer, traiter et transcrire des segments audio en temps réel en utilisant l'API **Whisper**. Ce projet est modulaire et fournit des fonctionnalités avancées telles que la capture d'audio, la segmentation de la parole, la mise en file d'attente des segments audio, et la transcription automatique.

## Fonctionnalités principales (C#)

- **Capture audio en temps réel** : Capture des segments audio à partir d'une source telle qu'un microphone.
- **Segmentation audio** : Détection d'activité vocale (VAD) pour séparer les segments de parole des périodes de silence.
- **Transcription automatique** : Utilise le modèle **Whisper** pour convertir les segments audio en texte.
- **Mise en file d'attente** : Gestion de la file d'attente des segments audio en attente de transcription.
- **Événements** : Déclenchement d'événements lors de la fin de la transcription ou la détection de silence.

## Architecture du projet

### Classes principales

#### `AudioCapturer`
- **Rôle** : Gère la capture des données audio à partir d'une source.

#### `SpeechSegmenter`
- **Rôle** : Analyse les segments audio pour détecter la parole et le silence. Accumule les échantillons audio lorsqu'une activité vocale est détectée, et complète le segment lorsque le silence est détecté.

#### `TranscriptionQueue`
- **Rôle** : Gère la file d'attente des segments audio à transcrire.

#### `TranscriptionWorker`
- **Rôle** : Transcrit les segments audio en texte en utilisant l'API **Whisper** et déclenche des événements une fois la transcription terminée.

#### `VadDetector`
- **Rôle** : Détecte l'activité vocale (VAD) dans les segments audio, permettant au **SpeechSegmenter** de différencier la parole du silence.

#### `VoiceFilter`
- **Rôle** : Filtre les fréquences audio pour isoler la voix humaine et supprimer les bruits.

## Fonctionnement global

1. **Capture audio** : Les segments audio sont capturés en temps réel via la classe `AudioCapturer`.
2. **Segmentation de la parole** : Les échantillons sont analysés par le `SpeechSegmenter` qui détecte les segments de parole et de silence en utilisant le `VadDetector`.
3. **Mise en file d'attente** : Les segments de parole sont ajoutés à la `TranscriptionQueue` pour être transcrits ultérieurement.
4. **Transcription automatique** : Le `TranscriptionWorker` consomme les segments audio de la file d'attente et les transcrit en texte avec **Whisper**.
5. **Événements** : Des événements sont déclenchés à la fin de la transcription pour traiter le texte transcrit ou indiquer la fin d'un segment.

## Exigences du système

- **C#**
- **.NET Core 8.0 ou supérieur**
- **Whisper API**

## Installation

1. Clonez le dépôt du projet :

    ```bash
    git clone https://github.com/enixame/SpeechFlow.git
    ```

2. Installez les dépendances nécessaires.

3. Lancez le projet en utilisant Visual Studio ou en ligne de commande avec :

    ```bash
    dotnet build
    dotnet run
    ```

## Contribuer

Les contributions sont les bienvenues. Veuillez soumettre une issue avant d'envoyer une pull request.


