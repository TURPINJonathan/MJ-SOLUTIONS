import { Component, OnInit, Renderer2 } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-switch-mode',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './switch-mode.html',
  styleUrls: ['./switch-mode.scss']
})
export class SwitchModeComponent implements OnInit {
  isDark = false;

  constructor(private renderer: Renderer2) {}

  ngOnInit() {
		if (localStorage.getItem('theme') === 'dark') {
			document.body.setAttribute('data-theme', 'dark');
      this.isDark = true;
    } else {
      document.body.setAttribute('data-theme', 'light');
			this.isDark = false;
    }
  }

  toggleTheme() {
    this.isDark = !this.isDark;
    if (this.isDark) {
			document.body.setAttribute('data-theme', 'dark');
      localStorage.setItem('theme', 'dark');
    } else {
      document.body.setAttribute('data-theme', 'light');
      localStorage.setItem('theme', 'light');
    }
  }
}