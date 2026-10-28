import { useEffect, useState } from "react"
import axios from "axios"
import { toast } from "react-toastify"
import { TodoList } from "@/components/TodoList"
import { ListSelector } from "@/components/ListSelector"
import { createList, deleteList, getLists, renameList } from "@/api"
import type { GroceryListSummary } from "@/api"
import { getSelectedListId, setSelectedListId } from "@/utils/list"
import "./styles.css"

const getErrorMessage = (error: unknown, fallback: string): string => {
    if (axios.isAxiosError(error) && error.response?.status === 400) {
        const message = error.response.data?.message
        if (typeof message === 'string' && message) return message
    }
    return fallback
}

export const Home = () => {
    const [lists, setLists] = useState<GroceryListSummary[]>([])
    const [selectedListId, setSelectedId] = useState<number | null>(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(false)
    const [reloadKey, setReloadKey] = useState(0)

    useEffect(() => {
        let cancelled = false
        const load = async () => {
            setLoading(true)
            setError(false)
            try {
                const data = await getLists()
                if (cancelled) return
                const saved = getSelectedListId()
                const initial = data.find(list => list.id === saved) ?? data[0]
                setLists(data)
                setSelectedId(initial ? initial.id : null)
                if (initial) setSelectedListId(initial.id)
            } catch (err) {
                if (cancelled) return
                console.error('Error loading lists:', err)
                setError(true)
                toast.error('Failed to load lists. Please try again.')
            } finally {
                if (!cancelled) setLoading(false)
            }
        }
        load()
        return () => { cancelled = true }
    }, [reloadKey])

    const selectList = (listId: number) => {
        setSelectedId(listId)
        setSelectedListId(listId)
    }

    const handleCreate = async (name: string): Promise<boolean> => {
        try {
            const created = await createList(name)
            setLists(prev => [...prev, created])
            selectList(created.id)
            toast.success('List created!')
            return true
        } catch (err) {
            console.error('Error creating list:', err)
            toast.error(getErrorMessage(err, 'Failed to create list. Please try again.'))
            return false
        }
    }

    const handleRename = async (listId: number, name: string): Promise<boolean> => {
        try {
            const renamed = await renameList(listId, name)
            setLists(prev => prev.map(list => list.id === listId ? renamed : list))
            toast.success('List renamed!')
            return true
        } catch (err) {
            console.error('Error renaming list:', err)
            toast.error(getErrorMessage(err, 'Failed to rename list. Please try again.'))
            return false
        }
    }

    const handleDelete = async (listId: number): Promise<boolean> => {
        try {
            await deleteList(listId)
            const remaining = lists.filter(list => list.id !== listId)
            setLists(remaining)
            if (listId === selectedListId && remaining.length > 0) {
                selectList(remaining[0].id)
            }
            toast.success('List deleted!')
            return true
        } catch (err) {
            console.error('Error deleting list:', err)
            toast.error(getErrorMessage(err, 'Failed to delete list. Please try again.'))
            return false
        }
    }

    return (
      <div className="home-container">
        <div className="home-content">
          {loading && <p className="home-message">Loading lists...</p>}

          {error && (
            <div className="home-message">
              <span>Failed to load lists.</span>
              <button onClick={() => setReloadKey(key => key + 1)} className="home-retry-button">
                Retry
              </button>
            </div>
          )}

          {!loading && !error && selectedListId !== null && (
            <>
              <ListSelector
                lists={lists}
                selectedListId={selectedListId}
                onSelect={selectList}
                onCreate={handleCreate}
                onRename={handleRename}
                onDelete={handleDelete}
              />
              <TodoList listId={selectedListId} />
            </>
          )}
        </div>
      </div>
  )
}
