import axios from 'axios'

// Get API URL from environment variable with fallback
const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:8000'

export const api = axios.create({
  baseURL: apiUrl,
})

export interface GroceryListSummary {
    id: number
    name: string
    createdAt: string
    updatedAt: string
}

export const getLists = async (): Promise<GroceryListSummary[]> => {
    const response = await api.get('/api/lists')
    return response.data
}

export const createList = async (name: string): Promise<GroceryListSummary> => {
    const response = await api.post('/api/lists', { name })
    return response.data
}

export const renameList = async (id: number, name: string): Promise<GroceryListSummary> => {
    const response = await api.put(`/api/lists/${id}`, { name })
    return response.data
}

export const deleteList = async (id: number) => {
    const response = await api.delete(`/api/lists/${id}`)
    return response.data
}

export const getGroceryList = async (listId: number) => {
    const response = await api.get('/api/items', { params: { listId } })
    return response.data
}

export const addItem = async (description: string, listId: number) => {
    const response = await api.post('/api/items', { description, listId })
    return response.data
}

export const deleteItem = async (id: number) => {
    const response = await api.delete(`/api/items/${id}`)
    return response.data
}

export const markItemAsChecked = async (id: number, checked: boolean) => {
    const response = await api.patch(`/api/items/${id}/checked`, { checked })
    return response.data
}

export const chat = async(message: string, listId: number, onChunk?: (chunk: string) => void) => {
    try {
        const response = await fetch(`${apiUrl}/api/chat`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ message, listId })
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
