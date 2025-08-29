import { User } from '#models/user.model';
import { createAction, props } from '@ngrx/store';

export const setUser = createAction('[User] Set User', props<{ user: User }>());
export const clearUser = createAction('[User] Clear User');