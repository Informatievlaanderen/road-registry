export default {
  format(iso: string): string {
    const d = new Date(iso); // parses the offset correctly
    const day = d.getDate(); // 'd' -> no leading zero
    const month = String(d.getMonth() + 1).padStart(2, "0"); // 'MM'
    const year = d.getFullYear(); // 'yyyy'
    const hh = String(d.getHours()).padStart(2, "0"); // 'HH'
    const mm = String(d.getMinutes()).padStart(2, "0"); // 'mm'
    return `${day}/${month}/${year} ${hh}:${mm}`;
  },
};
