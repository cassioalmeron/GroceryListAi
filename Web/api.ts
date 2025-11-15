import axios from 'axios'

// Get API URL from environment variable with fallback
console.log('VITE_API_URL:', import.meta.env.VITE_API_URL)
const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:8000'

const api = axios.create({
  baseURL: apiUrl,
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
    try {
        const response = await fetch(`${apiUrl}/chat`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ message })
        })

        if (!response.ok) {
            const errorText = await response.text()
            throw new Error(`Chat request failed: ${response.status} - ${errorText}`)
        }

        if (!response.body) {
            throw new Error('Response body is empty')
        }

        const reader = response.body.getReader()
        const decoder = new TextDecoder('utf-8')
        let buffer = ''

        try {
            while (true) {
                const { done, value } = await reader.read()

                if (value) {
                    buffer += decoder.decode(value, { stream: true })
                }

                if (done) {
                    // Flush any remaining data in the buffer
                    const final = decoder.decode()
                    if (final) {
                        buffer += final
                    }
                    break
                }

                // Process complete chunks from buffer
                while (buffer.length > 0) {
                    if (onChunk) {
                        onChunk(buffer)
                        buffer = ''
                    } else {
                        break
                    }
                }
            }
        } finally {
            reader.releaseLock()
        }
    } catch (error) {
        console.error('Chat API Error:', error)
        throw error
    }
}

export default api