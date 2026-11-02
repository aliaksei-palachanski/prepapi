import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { ApiError, createTodo, getTodos } from './api';
import type { Todo } from './api';

export function TodoList() {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [title, setTitle] = useState('');

  useEffect(() => {
    getTodos()
      .then(setTodos)
      .catch(() => setError('Could not load todos'))
      .finally(() => setLoading(false));
  }, []);

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    try {
      const created = await createTodo(title.trim());
      setTodos(prev => [...prev, created]);
      setTitle('');
    } catch (err) {
      setError(
        err instanceof ApiError && err.status === 409
          ? 'This todo already exists'
          : 'Could not create todo',
      );
    }
  }

  if (loading) return <p>Loading...</p>;

  return (
    <section>
      <h1>Todos</h1>
      {error && <p role="alert">{error}</p>}
      <form onSubmit={handleSubmit}>
        <label htmlFor="title">New todo</label>
        <input id="title" value={title} onChange={e => setTitle(e.target.value)} />
        <button type="submit" disabled={title.trim() === ''}>Add</button>
      </form>
      <ul>
        {todos.map(t => (
          <li key={t.id}>
            {t.title}
            {t.isDone && <span> ✓</span>}
          </li>
        ))}
      </ul>
    </section>
  );
}