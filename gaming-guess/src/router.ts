import { useEffect, useRef, useState } from 'react';

export function currentPath(): string {
  const hash = window.location.hash.replace(/^#/, '');
  return hash || '/';
}

export function navigate(path: string): void {
  if (currentPath() === path) return;
  window.location.hash = path;
}

export function useRoute(): string {
  const [path, setPath] = useState(currentPath);
  useEffect(() => {
    const onChange = () => setPath(currentPath());
    window.addEventListener('hashchange', onChange);
    return () => window.removeEventListener('hashchange', onChange);
  }, []);
  const firstRender = useRef(true);
  useEffect(() => {
    // Move focus to the page heading on navigation so screen reader and
    // keyboard users are not left at the top of a stale document. Skipped on
    // first paint, where nobody has navigated and a focus ring would be noise.
    if (firstRender.current) {
      firstRender.current = false;
      return;
    }
    document.querySelector<HTMLElement>('main h1')?.focus();
    window.scrollTo(0, 0);
  }, [path]);
  return path;
}

export function segments(path: string): string[] {
  return path.split('/').filter(Boolean);
}
