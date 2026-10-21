import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { loadingInterceptor } from './loading.interceptor';
import { LoadingService } from './loading.service';

describe('loadingInterceptor (S:UI-General)', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([loadingInterceptor])), provideHttpClientTesting()],
    });
  });

  it('is loading while a request runs and stops when it ends', () => {
    const loading = TestBed.inject(LoadingService);
    const http = TestBed.inject(HttpTestingController);
    TestBed.inject(HttpClient).get('/api/claims').subscribe();

    const runningState = loading.isLoading();
    http.expectOne('/api/claims').flush([]);

    expect(runningState).toBe(true);
    expect(loading.isLoading()).toBe(false);
  });
});
