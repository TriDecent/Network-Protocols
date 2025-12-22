import type { JSX } from 'react';
import {
  IoDesktopOutline,
  IoPhonePortraitOutline,
  IoTabletPortraitOutline,
} from 'react-icons/io5';

import { RiComputerLine } from 'react-icons/ri';

interface IClientInfoRetriever {
  getDeviceIcon(userAgent?: string | null): JSX.Element;
  getBrowserInfo(userAgent?: string | null): string;
  getOSInfo(userAgent?: string | null): string;
  getDeviceType(userAgent?: string | null): string;
}

class ClientInfoRetriever implements IClientInfoRetriever {
  getDeviceIcon(userAgent?: string | null) {
    if (!userAgent) return <RiComputerLine />;

    const ua = userAgent.toLowerCase();
    const device = this.getDeviceType(ua);

    if (device === 'Mobile') return <IoPhonePortraitOutline />;
    if (device === 'Tablet') return <IoTabletPortraitOutline />;

    return <IoDesktopOutline />;
  }

  getBrowserInfo(userAgent?: string | null) {
    if (!userAgent) return 'Unknown Browser';
    const ua = userAgent;

    if (/EdgA?\//.test(ua) || /EdgiOS/.test(ua) || /Edg\//.test(ua))
      return 'Edge';
    if (/OPR\//.test(ua) || /Opera/.test(ua)) return 'Opera';
    if (/FxiOS\//.test(ua) || /Firefox\//.test(ua)) return 'Firefox';
    if (/CriOS\//.test(ua)) return 'Chrome';
    if (/Chrome\//.test(ua) && !/Chromium/.test(ua)) {
      return /Mobile/.test(ua) ? 'Chrome Mobile' : 'Chrome';
    }
    if (/Safari\//.test(ua) && !/Chrome|Chromium|OPR|Edg/.test(ua))
      return 'Safari';

    return 'Unknown Browser';
  }

  getOSInfo(userAgent?: string | null) {
    if (!userAgent) return 'Unknown OS';
    const ua = userAgent;

    if (/Windows Phone/i.test(ua)) return 'Windows Phone';
    if (/Windows NT/i.test(ua)) return 'Windows';
    if (/Android/i.test(ua)) return 'Android';
    if (/(iPhone|iPad|iPod|CPU iPhone OS|CPU OS)/i.test(ua)) return 'iOS';
    if (/(Macintosh|Mac OS X)/i.test(ua)) return 'macOS';
    if (/CrOS/i.test(ua)) return 'ChromeOS';
    if (/Linux/i.test(ua)) return 'Linux';

    return 'Unknown OS';
  }

  getDeviceType(userAgent?: string | null) {
    if (!userAgent) return 'Desktop';
    const ua = userAgent;

    if (/(iPad|Tablet|SM-T|Tab)/i.test(ua)) return 'Tablet';
    if (/(Mobi|Mobile|iPhone|iPod|Android.*Mobile)/i.test(ua)) return 'Mobile';
    if (/Android/i.test(ua) && !/Mobile/i.test(ua)) return 'Tablet';

    return 'Desktop';
  }
}

export const clientInfoRetriever: IClientInfoRetriever =
  new ClientInfoRetriever();
