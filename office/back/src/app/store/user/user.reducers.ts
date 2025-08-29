import { createReducer, on } from '@ngrx/store';
import { setUser, clearUser } from './user.actions';
import { User } from '#models/user.model';

export interface UserState {
  user: User | null;
}

export const initialState: UserState = {
  user: null
};

export const userReducer = createReducer(
  initialState,
  on(setUser, (state, { user }) => ({ ...state, user })),
  on(clearUser, state => ({ ...state, user: null }))
);