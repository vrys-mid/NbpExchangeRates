export const formatDate = (isoDate: string): string => {
  return new Date(isoDate).toLocaleDateString("pl-PL", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
};