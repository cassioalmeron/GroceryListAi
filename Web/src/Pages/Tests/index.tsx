import { useState, useEffect, useRef } from 'react';
import './styles.css';

// Type declaration for Speech Recognition API
interface SpeechRecognitionResult {
  [index: number]: SpeechRecognitionAlternative;
  length: number;
  isFinal: boolean;
}

interface SpeechRecognitionAlternative {
  transcript: string;
  confidence: number;
}

interface SpeechRecognitionEvent {
  resultIndex: number;
  results: SpeechRecognitionResult[];
}

interface SpeechRecognitionErrorEvent {
  error: string;
  message: string;
}

interface SpeechRecognition extends EventTarget {
  continuous: boolean;
  interimResults: boolean;
  lang: string;
  start(): void;
  stop(): void;
  onresult: ((event: SpeechRecognitionEvent) => void) | null;
  onerror: ((event: SpeechRecognitionErrorEvent) => void) | null;
  onend: (() => void) | null;
}

declare global {
  interface Window {
    SpeechRecognition: new () => SpeechRecognition;
    webkitSpeechRecognition: new () => SpeechRecognition;
  }
}

export const Tests = () => {
  const [stream, setStream] = useState<MediaStream | null>(null);
  const [isListening, setIsListening] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [audioLevel, setAudioLevel] = useState(0);
  const [transcription, setTranscription] = useState('');
  const [interimTranscript, setInterimTranscript] = useState('');
  const [speechSupported, setSpeechSupported] = useState(false);
  
  const audioContextRef = useRef<AudioContext | null>(null);
  const analyserRef = useRef<AnalyserNode | null>(null);
  const animationFrameRef = useRef<number | null>(null);
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const recognitionRef = useRef<SpeechRecognition | null>(null);

  // Check if browser supports getUserMedia and Speech Recognition
  useEffect(() => {
    // Check if browser supports speech recognition
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;

    if (!SpeechRecognition) {
      setError('Your browser does not support speech recognition. Please use Chrome or Edge.');
      return;
    }

    setSpeechSupported(true);

    // Initialize speech recognition
    const recognition = new SpeechRecognition();
    recognition.continuous = true;
    recognition.interimResults = true;
    recognition.lang = 'en-US';

    recognition.onresult = (event: any) => {
      let interim = '';
      let final = '';

      for (let i = event.resultIndex; i < event.results.length; i++) {
        const transcript = event.results[i][0].transcript;
        if (event.results[i].isFinal) {
          final += transcript + ' ';
        } else {
          interim += transcript;
        }
      }

      if (final) {
        setTranscription(prev => prev + final);
      }
      setInterimTranscript(interim);
    };

    recognition.onerror = (event: any) => {
      console.error('Speech recognition error:', event.error);
      if (event.error === 'no-speech') {
        console.log('No speech detected. Please try again.');
      }
      setError(`Speech recognition error: ${event.error}`);
    };

    recognition.onend = () => {
      setIsListening(false);
      setInterimTranscript('');
    };

    recognitionRef.current = recognition;

    return () => {
      if (recognitionRef.current) {
        recognitionRef.current.stop();
      }
    };
  }, []);

  // Start microphone access and speech recognition
  const startMicrophone = async () => {
    try {
      setError(null);
      
      // Start speech recognition
      if (recognitionRef.current && !isListening) {
        setTranscription('');
        setInterimTranscript('');
        recognitionRef.current.start();
        setIsListening(true);
      }

      // Get microphone access for visualization
      const audioStream = await navigator.mediaDevices.getUserMedia({
        audio: {
          echoCancellation: true,
          noiseSuppression: true,
          autoGainControl: true
        }
      });
      
      setStream(audioStream);

      // Create audio context for audio processing
      audioContextRef.current = new (window.AudioContext || (window as any).webkitAudioContext)();
      const source = audioContextRef.current.createMediaStreamSource(audioStream);
      analyserRef.current = audioContextRef.current.createAnalyser();
      
      analyserRef.current.fftSize = 256;
      analyserRef.current.smoothingTimeConstant = 0.8;
      
      source.connect(analyserRef.current);
      
      // Start audio visualization
      startVisualization();

    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
      setError(errorMessage);
      console.error('Error accessing microphone:', err);
    }
  };

  // Stop microphone access and speech recognition
  const stopMicrophone = () => {
    // Stop speech recognition
    if (recognitionRef.current && isListening) {
      recognitionRef.current.stop();
      setIsListening(false);
    }

    // Stop microphone stream
    if (stream) {
      stream.getTracks().forEach(track => track.stop());
      setStream(null);
    }
    
    if (audioContextRef.current) {
      audioContextRef.current.close();
    }
    
    if (animationFrameRef.current) {
      cancelAnimationFrame(animationFrameRef.current);
    }
  };

  // Audio visualization
  const startVisualization = () => {
    if (!analyserRef.current || !canvasRef.current) return;

    const canvas = canvasRef.current;
    const canvasCtx = canvas.getContext('2d');
    const analyser = analyserRef.current;
    
    if (!canvasCtx) return;

    const bufferLength = analyser.frequencyBinCount;
    const dataArray = new Uint8Array(bufferLength);

    const draw = () => {
      animationFrameRef.current = requestAnimationFrame(draw);

      analyser.getByteFrequencyData(dataArray);

      // Calculate average audio level
      let sum = 0;
      for (let i = 0; i < bufferLength; i++) {
        sum += dataArray[i];
      }
      const average = sum / bufferLength;
      setAudioLevel(average);

      // Clear canvas
      canvasCtx.fillStyle = '#000';
      canvasCtx.fillRect(0, 0, canvas.width, canvas.height);

      // Draw frequency bars
      const barWidth = (canvas.width / bufferLength) * 2.5;
      let barHeight;
      let x = 0;

      for (let i = 0; i < bufferLength; i++) {
        barHeight = (dataArray[i] / 255) * canvas.height;

        // Create gradient for bars
        const gradient = canvasCtx.createLinearGradient(0, canvas.height, 0, canvas.height - barHeight);
        gradient.addColorStop(0, '#ff0000');
        gradient.addColorStop(0.5, '#ffff00');
        gradient.addColorStop(1, '#00ff00');

        canvasCtx.fillStyle = gradient;
        canvasCtx.fillRect(x, canvas.height - barHeight, barWidth, barHeight);

        x += barWidth + 1;
      }
    };

    draw();
  };

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      stopMicrophone();
    };
  }, []);

  return (
    <div className="tests-container">
      <h1 className="tests-title">
        🎤 Microphone Test
      </h1>
      
      {!speechSupported && (
        <div className="browser-warning">
          ⚠️ Your browser doesn't support speech recognition. Please use Chrome or Edge.
        </div>
      )}

      {error && (
        <div className="error-message">
          ❌ Error: {error}
        </div>
      )}

      <div className="controls-section">
        <button
          onClick={isListening ? stopMicrophone : startMicrophone}
          disabled={!speechSupported}
          className={`microphone-button ${isListening ? 'stop' : 'start'}`}
        >
          {isListening ? '🛑 Stop Recognition' : '🎤 Start Recognition'}
        </button>

        {isListening && (
          <div className="listening-status">
            🔊 Listening... Audio Level: {Math.round(audioLevel)}
          </div>
        )}
      </div>

      {/* Transcription Display */}
      {(transcription || interimTranscript) && (
        <div className="instructions-card">
          <h3 className="instructions-title">
            📝 Speech Recognition Results:
          </h3>
          <div style={{ 
            backgroundColor: '#f8f9fa', 
            padding: '15px', 
            borderRadius: '8px',
            border: '1px solid #dee2e6',
            minHeight: '100px'
          }}>
            {transcription && (
              <div style={{ marginBottom: interimTranscript ? '15px' : '0' }}>
                <strong style={{ color: '#2c3e50', display: 'block', marginBottom: '5px' }}>
                  Final Text:
                </strong>
                <p style={{ 
                  color: '#495057', 
                  lineHeight: '1.6',
                  margin: '0',
                  fontSize: '16px'
                }}>
                  {transcription}
                </p>
              </div>
            )}
            {interimTranscript && (
              <div>
                <strong style={{ color: '#2c3e50', display: 'block', marginBottom: '5px' }}>
                  Live Transcription:
                </strong>
                <p style={{ 
                  color: '#6c757d', 
                  fontStyle: 'italic',
                  lineHeight: '1.6',
                  margin: '0',
                  fontSize: '16px'
                }}>
                  {interimTranscript}
                </p>
              </div>
            )}
          </div>
        </div>
      )}

      {isListening && (
        <div className="visualization-section">
          <h3 className="visualization-title">
            📊 Audio Visualization
          </h3>
          <div className="visualization-container">
            <canvas
              ref={canvasRef}
              width={800}
              height={200}
              className="visualization-canvas"
            />
          </div>
        </div>
      )}

      <div className="instructions-card">
        <h3 className="instructions-title">
          📋 Instructions:
        </h3>
        <ul className="instructions-list">
          <li className="instructions-item">Click <strong>"Start Recognition"</strong> to begin speech recognition</li>
          <li className="instructions-item">Your browser will ask for microphone permission</li>
          <li className="instructions-item">Speak clearly into your microphone</li>
          <li className="instructions-item">Watch the live transcription appear as you speak</li>
          <li className="instructions-item">Final text will be saved when you pause speaking</li>
          <li className="instructions-item">The audio visualization shows frequency ranges of your voice</li>
        </ul>
      </div>

      <div className="technical-card">
        <h3 className="technical-title">
          🔧 Technical Details:
        </h3>
        <ul className="technical-list">
          <li className="technical-item">
            <strong className="technical-label">Speech Recognition:</strong> 
            <span className={speechSupported ? 'status-active' : 'status-inactive'}>
              {speechSupported ? '✅ Supported' : '❌ Not supported'}
            </span>
          </li>
          <li className="technical-item">
            <strong className="technical-label">Recognition Status:</strong> 
            <span className={isListening ? 'status-active' : 'status-inactive'}>
              {isListening ? '✅ Listening' : '❌ Stopped'}
            </span>
          </li>
          <li className="technical-item">
            <strong className="technical-label">Stream Status:</strong> 
            <span className={stream ? 'status-active' : 'status-inactive'}>
              {stream ? '✅ Active' : '❌ Inactive'}
            </span>
          </li>
          <li className="technical-item">
            <strong className="technical-label">Audio Context:</strong> 
            <span className={audioContextRef.current ? 'status-active' : 'status-inactive'}>
              {audioContextRef.current ? '✅ Created' : '❌ Not created'}
            </span>
          </li>
          <li className="technical-item">
            <strong className="technical-label">Current Audio Level:</strong> 
            <span className="audio-level-badge">
              {Math.round(audioLevel)}
            </span>
          </li>
          <li className="technical-item">
            <strong className="technical-label">Words Transcribed:</strong> 
            <span className="audio-level-badge">
              {transcription.split(' ').filter(word => word.length > 0).length}
            </span>
          </li>
        </ul>
      </div>
    </div>
  );
};