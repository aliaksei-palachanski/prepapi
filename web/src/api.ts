export type Todo = { id: number; title: string; isDone: boolean };

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

export async function getTodos(): Promise<Todo[]> {
  const res = await fetch('/todos');
  if (!res.ok) throw new ApiError(res.status, 'Failed to load todos');
  return res.json();
}

export async function createTodo(title: string): Promise<Todo> {
  const res = await fetch('/todos', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ title, isDone: false }),
  });
  if (!res.ok) throw new ApiError(res.status, 'Failed to create todo');
  return res.json();
}