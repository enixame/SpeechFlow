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

### Python

1. Installez les dépendances Python en utilisant le fichier `requirements.txt` :
   ```bash
   pip install -r python/requirements.txt
