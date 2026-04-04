import { environment } from '../../environments';

export function convertUTCToIST(isoDate: Date | string | null | undefined): string {
    if (!isoDate) {
      return '';
    }
    const isoString = typeof isoDate === 'string' ? isoDate + 'Z' : isoDate.toISOString();
    const utcDate = new Date(isoString);
    return utcDate.toLocaleString("en-IN", {
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      year: "numeric",
      month: "short",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      hour12: true
    });
  }

export function resolveAssetUrl(assetPath: string | null | undefined): string {
  if (!assetPath) {
    return '';
  }

  const normalizedPath = assetPath.replace(/\\/g, '/');
  if (/^https?:\/\//i.test(normalizedPath)) {
    return normalizedPath;
  }

  const uploadsIndex = normalizedPath.lastIndexOf('/Uploads/');
  if (uploadsIndex >= 0) {
    return `${getApiOrigin()}${normalizedPath.substring(uploadsIndex)}`;
  }

  if (normalizedPath.startsWith('/Uploads/')) {
    return `${getApiOrigin()}${normalizedPath}`;
  }

  return normalizedPath;
}

function getApiOrigin(): string {
  const apiUrl = environment.apiUrl ?? '';
  return apiUrl.replace(/\/api\/?$/i, '');
}
