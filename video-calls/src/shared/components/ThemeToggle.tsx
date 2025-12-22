'use client';

import { Tabs, TabsList, TabsTrigger } from '@/shared/components/ui';
import { Monitor, Moon, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import { useSyncExternalStore } from 'react';

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();

  const mounted = useSyncExternalStore(
    () => () => {},
    () => true,
    () => false
  );

  const themes = [
    { value: 'light', icon: Sun, label: 'Light' },
    { value: 'dark', icon: Moon, label: 'Dark' },
    { value: 'system', icon: Monitor, label: 'System' },
  ] as const;

  return mounted ? (
    <Tabs value={theme} onValueChange={setTheme}>
      <TabsList>
        {themes.map(({ value, icon: Icon, label }) => (
          <TabsTrigger key={value} value={value} title={label}>
            <Icon className='size-4' />
          </TabsTrigger>
        ))}
      </TabsList>
    </Tabs>
  ) : null;
}
