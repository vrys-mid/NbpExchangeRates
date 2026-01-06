import { apiClient } from "./client";
import type { CurrencyRate } from "../types/CurrencyRate";

export const fetchLatestRates = async (): Promise<CurrencyRate[]> => {
    const response = await apiClient.get<CurrencyRate[]>("/rates/latest");
    return response.data;
};

export const fetchPublicationDates = async (): Promise<string[]> => {
  const res = await apiClient.get("/rates/publication-dates");
  return res.data;
};

export const fetchRatesByDate = async (date?: string) => {
  const res = await apiClient.get("/rates", {
    params: date ? { date } : {},
  });
  return res.data;
};
