import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:8000',
})

export const getGroceryList = async () => {
    const response = await api.get('/items')
    console.log(response.data)
    return response.data
}

export const addItem = async (description: string) => {
    const response = await api.post('/items', { description })
    console.log(response.data)
    return response.data
}

export const deleteItem = async (id: number) => {
    const response = await api.delete(`/items/${id}`)
    console.log(response.data)
    return response.data
}

export const markItemAsChecked = async (id: number, checked: boolean) => {
    const response = await api.patch(`/items/${id}/checked`, { checked })
    console.log(response.data)
    return response.data
}

export const chat = async(message: string, onChunk?: (chunk: string) => void) => {
    const response = await fetch('http://localhost:8000/chat', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ message })
    })

    if (!response.ok) {
        throw new Error('Chat request failed')
    }

    const reader = response.body?.getReader()
    if (!reader) {
        throw new Error('Failed to get response reader')
    }

    const decoder = new TextDecoder()

    try {
        while (true) {
            const { done, value } = await reader.read()
            if (done) break

            const chunk = decoder.decode(value, { stream: true })
            
            // Check for errors in the stream
            if (chunk.startsWith('Error:')) {
                throw new Error(chunk)
            }
            
            // Send each chunk directly to the callback
            if (onChunk && chunk) {
                onChunk(chunk)
            }
        }
    } finally {
        reader.releaseLock()
    }
}

export default api