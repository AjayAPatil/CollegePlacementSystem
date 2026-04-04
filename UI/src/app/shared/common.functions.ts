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