import { apiClient } from "./client";
import type { CurrencyRate } from "../types/CurrencyRate";

export const fetchPublicationDates = async (): Promise<string[]> => {
  const res = await apiClient.get("/rates/publication-dates");
  return res.data;
};

export const fetchRatesByDate = async (date?: string | null) => {
  const res = await apiClient.get<CurrencyRate[]>("/rates", {
    params: date ? { date } : {},
  });
  return res.data;
};
