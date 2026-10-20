import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { APP_CONFIG } from '../../core/config/app.constants';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FooterComponent {
  protected readonly appConfig = APP_CONFIG;
  protected readonly auth = inject(AuthService);
  protected readonly currentYear = new Date().getFullYear();
}
