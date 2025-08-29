import { createFeatureSelector, createSelector } from '@ngrx/store';
import { UserState } from './user.reducers';

export const selectUserState = createFeatureSelector<UserState>('user');

export const selectUser = createSelector(
  selectUserState,
  (state) => state.user
);

export const selectUserPermissions = createSelector(
  selectUser,
  (user) => user ? user.Permissions : []
);

export const selectUserRole = createSelector(
  selectUser,
  (user) => user ? user.Role : null
);