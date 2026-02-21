import React, { useState, useRef, useEffect } from 'react';
import './style.css';
import { chat } from '../../../api';
import VoiceButton from '../VoiceButton';

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
  const messagesEndRef = useRef<HTMLDivElement>(null);

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
            disabled={isTyping}
          />

          <VoiceButton
            onTranscript={(text) => handleSendMessageWithText(text)}
            disabled={isTyping}
          />

          <button
            onClick={handleSendMessage}
            disabled={inputValue.trim() === '' || isTyping}
            className="send-button"
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="22" y1="2" x2="11" y2="13"></line>
              <polygon points="22,2 15,22 11,13 2,9"></polygon>
            </svg>
          </button>
        </div>
      </div>
    </div>
  );
};

export default Chat;
