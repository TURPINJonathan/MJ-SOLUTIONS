import { Component } from '@angular/core';
import { SwitchModeComponent } from '#ui/switch-mode/switch-mode';

@Component({
  selector: 'app-header',
  imports: [
		SwitchModeComponent,
	],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class HeaderLayout {

}
