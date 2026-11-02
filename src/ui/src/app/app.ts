import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterOutlet } from '@angular/router';

// The app shell: a toolbar on every page, and the routed page below it.
@Component({
  selector: 'app-root',
  imports: [MatIconModule, MatToolbarModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
