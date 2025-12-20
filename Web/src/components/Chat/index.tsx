import React, { useState, useRef, useEffect } from 'react';
import './style.css';
import { chat } from '../../../api';
import { getLanguage } from '../../utils/language';

interface Message {
  id: number;
  text: string;
  sender: 'user' | 'ai';
  timestamp: Date;
}

interface ChatProps {
  onResponseComplete?: () => void;
}

const Chat: React.FC<ChatProps> = ({ onResponseComplete }) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [isListening, setIsListening] = useState(false);
  const [speechSupported, setSpeechSupported] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const recognitionRef = useRef<any>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const getAIResponse = async (userMessage: string, onChunk: (chunk: string) => void): Promise<void> => {
    try {
      await chat(userMessage, (chunk) => {
        if (chunk) {
          onChunk(chunk);
        }
      });
    } catch (error) {
      console.error('Error getting AI response:', error);
      throw error;
    }
  };

  const handleSendMessage = async () => {
    await handleSendMessageWithText(inputValue.trim());
  };

  const handleSendMessageWithText = async (text: string) => {
    if (text === '') return;

    const userMessage: Message = {
      id: Date.now(),
      text: text,
      sender: 'user',
      timestamp: new Date()
    };

    setMessages(prev => [...prev, userMessage]);
    setInputValue('');
    setIsTyping(true);

    try {
      // Create initial AI message for streaming
      const aiMessageId = Date.now() + 1;
      const initialAiMessage: Message = {
        id: aiMessageId,
        text: '',
        sender: 'ai',
        timestamp: new Date()
      };
      setMessages(prev => [...prev, initialAiMessage]);

      await getAIResponse(userMessage.text, (newChunk) => {
        // Append the new chunk to the AI message
        setMessages(prev => prev.map(msg =>
          msg.id === aiMessageId
            ? { ...msg, text: msg.text + newChunk }
            : msg
        ));
      });

      // Notify parent component that response is complete
      if (onResponseComplete) {
        onResponseComplete();
      }
    } catch (error) {
      console.error('Chat error:', error);
      const errorText = error instanceof Error ? error.message : 'Unknown error';
      const errorMessage: Message = {
        id: Date.now() + 2,
        text: `Error: ${errorText}. Please check your connection and try again.`,
        sender: 'ai',
        timestamp: new Date()
      };
      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsTyping(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const startVoiceRecording = () => {
    if (recognitionRef.current && !isListening && !isTyping) {
      setIsListening(true);
      recognitionRef.current.start();
    }
  };

  const stopVoiceRecording = () => {
    if (recognitionRef.current && isListening) {
      recognitionRef.current.stop();
      setIsListening(false);
    }
  };

  // Initialize speech recognition
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
      setInputValue(transcript);
      setIsListening(false);
      
      // Auto-send the transcribed message
      setTimeout(() => {
        handleSendMessageWithText(transcript);
      }, 100);
    };

    recognition.onerror = (event: any) => {
      console.error('Speech recognition error:', event.error);
      setIsListening(false);
      
      // Show error message
      if (event.error === 'not-allowed') {
        alert('Microphone permission denied. Please allow microphone access and try again.');
      } else if (event.error === 'no-speech') {
        alert('No speech detected. Please try speaking again.');
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

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <div className="chat-container">
      <div className="chat-header">
        <h2>AI Assistant</h2>
        <div className="status-indicator">
          <span className="status-dot"></span>
          Online
        </div>
      </div>

      <div className="chat-messages">
        {messages.length === 0 && (
          <div className="welcome-message">
            <h3>Welcome!</h3>
            <p>Start a conversation with the AI assistant. Ask me anything!</p>
          </div>
        )}
        
        {messages.map((message) => (
          <div
            key={message.id}
            className={`message ${message.sender === 'user' ? 'user-message' : 'ai-message'}`}
          >
            <div className="message-content">
              <div className="message-text">{message.text}</div>
              <div className="message-time">{formatTime(message.timestamp)}</div>
            </div>
          </div>
        ))}
        
        {isTyping && (
          <div className="message ai-message">
            <div className="message-content">
              <div className="typing-indicator">
                <span></span>
                <span></span>
                <span></span>
              </div>
            </div>
          </div>
        )}
        
        <div ref={messagesEndRef} />
      </div>

      <div className="chat-input-container">
        <div className="chat-input-wrapper">
          <textarea
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyPress={handleKeyPress}
            placeholder="Type your message here..."
            className="chat-input"
            rows={1}
            disabled={isTyping || isListening}
          />
          
          {/* Microphone Button */}
          <button
            onClick={isListening ? stopVoiceRecording : startVoiceRecording}
            disabled={isTyping || !speechSupported}
            className={`microphone-button ${isListening ? 'recording' : ''}`}
            title={isListening ? 'Stop recording' : 'Start voice recording'}
          >
            {isListening ? (
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <rect x="6" y="6" width="12" height="12" rx="2" ry="2"></rect>
                <rect x="11" y="11" width="2" height="2"></rect>
              </svg>
            ) : (
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 6 0V4a3 3 0 0 0-3-3z"></path>
                <path d="M19 10v2a7 7 0 0 1-14 0v-2"></path>
                <line x1="12" y1="19" x2="12" y2="23"></line>
                <line x1="8" y1="23" x2="16" y2="23"></line>
              </svg>
            )}
          </button>
          
          <button
            onClick={handleSendMessage}
            disabled={inputValue.trim() === '' || isTyping || isListening}
            className="send-button"
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="22" y1="2" x2="11" y2="13"></line>
              <polygon points="22,2 15,22 11,13 2,9"></polygon>
            </svg>
          </button>
        </div>
        
        {/* Recording Status */}
        {Boolean(isListening) && (
          <div className="recording-status">
            <div className="recording-indicator">
              <span></span>
              <span></span>
              <span></span>
            </div>
            <span>Listening... Speak now</span>
          </div>
        )}
      </div>
    </div>
  );
};

export default Chat;