/**
 * Formats a date string into a human-readable format
 */
export function formatDate(dateString: string): string {
    const date = new Date(dateString);
    
    // Check if date is valid
    if (isNaN(date.getTime())) {
      return 'Invalid date';
    }
    
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    }).format(date);
  }
  
  /**
   * Formats time in seconds to mm:ss format
   */
  export function formatTime(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    
    const formattedMinutes = String(minutes).padStart(2, '0');
    const formattedSeconds = String(remainingSeconds).padStart(2, '0');
    
    return `${formattedMinutes}:${formattedSeconds}`;
  }
  
  /**
   * Formats a percentage (adds % sign and rounds to whole number)
   */
  export function formatPercentage(value: number): string {
    return `${Math.round(value)}%`;
  }