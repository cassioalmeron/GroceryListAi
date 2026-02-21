import React, { useState, useRef, useEffect } from 'react';
import './style.css';
import { getLanguage } from '../../utils/language';
import { toast } from 'react-toastify';
import { MicrophoneIcon, StopRecordingIcon } from '../icons';

interface VoiceButtonProps {
  onTranscript: (text: string) => void;
  disabled?: boolean;
  className?: string;
}

const VoiceButton: React.FC<VoiceButtonProps> = ({ onTranscript, disabled, className }) => {
  const [isListening, setIsListening] = useState(false);
  const [speechSupported, setSpeechSupported] = useState(false);
  const recognitionRef = useRef<any>(null);

  useEffect(() => {
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;

    if (!SpeechRecognition) {
      console.warn('Speech recognition not supported');
      return;
    }

    setSpeechSupported(true);

    const recognition = new SpeechRecognition();
    recognition.continuous = false;
    recognition.interimResults = false;
    recognition.lang = getLanguage();
    recognition.maxAlternatives = 1;

    recognition.onresult = (event: any) => {
      const transcript = event.results[0][0].transcript;
      setIsListening(false);
      onTranscript(transcript);
    };

    recognition.onerror = (event: any) => {
      console.error('Speech recognition error:', event.error);
      setIsListening(false);

      if (event.error === 'not-allowed') {
        toast.error('Microphone permission denied. Please allow microphone access and try again.');
      } else if (event.error === 'no-speech') {
        toast.warning('No speech detected. Please try speaking again.');
      }
    };

    recognition.onend = () => {
      setIsListening(false);
    };

    recognitionRef.current = recognition;

    return () => {
      if (recognitionRef.current) {
        recognitionRef.current.stop();
      }
    };
  }, []);

  if (!speechSupported) {
    return null;
  }

  const handleClick = () => {
    if (!recognitionRef.current || disabled) return;

    if (isListening) {
      recognitionRef.current.stop();
      setIsListening(false);
    } else {
      setIsListening(true);
      recognitionRef.current.start();
    }
  };

  return (
    <>
      <button
        onClick={handleClick}
        disabled={disabled}
        className={`microphone-button ${isListening ? 'recording' : ''} ${className ?? ''}`}
        title={isListening ? 'Stop recording' : 'Start voice recording'}
      >
        {isListening ? (
          <StopRecordingIcon size={20} />
        ) : (
          <MicrophoneIcon size={20} />
        )}
      </button>

      {isListening && (
        <div className="recording-status">
          <div className="recording-indicator">
            <span></span>
            <span></span>
            <span></span>
          </div>
          <span>Listening... Speak now</span>
        </div>
      )}
    </>
  );
};

export default VoiceButton;
