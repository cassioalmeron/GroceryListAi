import os
from abc import ABC, abstractmethod
from openai import AsyncOpenAI
from ollama import AsyncClient
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

llm_type = os.getenv("LLM")
model = os.getenv("MODEL")
key = os.getenv("OPENAI_API_KEY")

SYSTEM_PROMPT = """
You are a helpful assistant that can help with grocery list software.
And the propose is to generate commands to be executed by the software, according to the user's request.

Possible commands:
- AddItem
- RemoveItem
- CheckItem
- UncheckItem

Answer format:
[{
    "command": "command",
    "value": "value"
}]

Examples of commands (only one command per answer):

[{
    "command": "AddItem",
    "value": "Milk"
}]

Examples of commands (more than one command per answer):
[{
    "command": "AddItem",
    "value": "Milk"
}, {
    "command": "AddItem",
    "value": "Bread"
}]

Important: The user may not follow exactly the format of the example, but the command and the value must be present.

Other topics out of the scope of the grocery list, besides the retrivial data, are not allowed.

The answer must be only just in JSON format, alwawys surround the JSON with [], no other text or comments.

if the user request is not a command, the answer must be:
[]

Some possible user's inputs beyond the obvious commands:
- check item x (CheckItem)
- uncheck item x (UncheckItem)
- conclude item x (CheckItem)
- unconclude item x (UncheckItem)
"""

# Validate required environment variables
if not model:
    raise ValueError(
        "MODEL environment variable is not set. Please set it in your .env file.\n"
        "For Ollama: MODEL=llama2\n"
        "For ChatGPT: MODEL=gpt-3.5-turbo"
    )

if llm_type == "chatgpt" and not key:
    raise ValueError(
        "OPENAI_API_KEY environment variable is required when LLM=chatgpt.\n"
        "Please set it in your .env file."
    )


# Abstract base class for LLM clients
class LLMClient(ABC):
    """Abstract base class for LLM clients"""
    
    def __init__(self, model: str):
        self.model = model
    
    @abstractmethod
    async def stream_chat(self, messages: list[dict]):
        """Stream chat responses from the LLM"""
        pass


# ChatGPT implementation
class ChatGPTClient(LLMClient):
    """OpenAI ChatGPT client implementation"""
    
    def __init__(self, model: str, api_key: str):
        super().__init__(model)
        self.client = AsyncOpenAI(api_key=api_key)
    
    async def stream_chat(self, messages: list[dict]):
        print("Using ChatGPT API")
        stream = await self.client.chat.completions.create(
            model=self.model,
            messages=messages,
            stream=True
        )
        
        async for chunk in stream:
            if chunk.choices[0].delta.content is not None:
                yield chunk.choices[0].delta.content


# Ollama implementation
class OllamaClient(LLMClient):
    """Ollama client implementation"""
    
    def __init__(self, model: str, host: str = "http://localhost:11434"):
        super().__init__(model)
        self.client = AsyncClient(host=host)
    
    async def stream_chat(self, messages: list[dict]):
        print("Using Ollama API")
        stream = await self.client.chat(
            model=self.model,
            messages=messages,
            stream=True
        )
        
        async for chunk in stream:
            if chunk.get("message") and chunk["message"].get("content"):
                yield chunk["message"]["content"]


# Factory function
def create_llm_client(llm_type: str, model: str, api_key: str = None) -> LLMClient:
    """Factory function to create the appropriate LLM client"""
    if llm_type == "chatgpt":
        if not api_key:
            raise ValueError("API key is required for ChatGPT")
        return ChatGPTClient(model, api_key)
    else:  # ollama
        return OllamaClient(model)


# Initialize the client
llm_client = create_llm_client(llm_type, model, key)


# Main function - now much simpler!
async def get_response(messages: list[dict]):
    """Get streaming response from the LLM"""
    # Add system prompt at the beginning
    messages.insert(0, {"role": "system", "content": SYSTEM_PROMPT})
    
    try:
        async for chunk in llm_client.stream_chat(messages):
            print(f"Chunk: {chunk}")
            yield chunk
    except Exception as e:
        print(f"Error in chat: {e}")
        import traceback
        traceback.print_exc()
        yield f"Error: {str(e)}\n\n"