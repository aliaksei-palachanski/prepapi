import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TodoList } from './TodoList';
import * as api from './api';

vi.mock('./api', async (importOriginal) => {
  const actual = await importOriginal<typeof import('./api')>();
  return { ...actual, getTodos: vi.fn(), createTodo: vi.fn() };
});

const getTodos = vi.mocked(api.getTodos);
const createTodo = vi.mocked(api.createTodo);

describe('TodoList', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('shows todos after loading', async () => {
    getTodos.mockResolvedValue([{ id: 1, title: 'Buy milk', isDone: false }]);

    render(<TodoList />);

    expect(screen.getByText('L1oading...')).toBeInTheDocument();
    expect(await screen.findByText('Buy milk')).toBeInTheDocument();
  });

  it('shows error when loading fails', async () => {
    getTodos.mockRejectedValue(new Error('network'));

    render(<TodoList />);

    expect(await screen.findByRole('alert')).toHaveTextContent('Could not load todos');
  });

  it('disables Add button for empty or whitespace input', async () => {
    getTodos.mockResolvedValue([]);
    const user = userEvent.setup();
    render(<TodoList />);

    const button = await screen.findByRole('button', { name: 'Add' });
    expect(button).toBeDisabled();

    await user.type(screen.getByLabelText('New todo'), '   ');
    expect(button).toBeDisabled();
  });

  it('adds a todo and clears the input', async () => {
    getTodos.mockResolvedValue([]);
    createTodo.mockResolvedValue({ id: 7, title: 'Learn RTL', isDone: false });
    const user = userEvent.setup();
    render(<TodoList />);

    await user.type(await screen.findByLabelText('New todo'), '  Learn RTL ');
    await user.click(screen.getByRole('button', { name: 'Add' }));

    expect(createTodo).toHaveBeenCalledWith('Learn RTL');
    expect(await screen.findByText('Learn RTL')).toBeInTheDocument();
    expect(screen.getByLabelText('New todo')).toHaveValue('');
  });

  it('shows conflict message on duplicate', async () => {
    getTodos.mockResolvedValue([]);
    createTodo.mockRejectedValue(new api.ApiError(409, 'Conflict'));
    const user = userEvent.setup();
    render(<TodoList />);

    await user.type(await screen.findByLabelText('New todo'), 'Buy milk');
    await user.click(screen.getByRole('button', { name: 'Add' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('This todo already exists');
  });
});