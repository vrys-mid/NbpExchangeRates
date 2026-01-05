import { apiClient } from "./client";
import type { CurrencyRate } from "../types/CurrencyRate";

export const fetchLatestRates = async (): Promise<CurrencyRate[]> => {
    const response = await apiClient.get<CurrencyRate[]>("/rates/latest");
    return response.data;
};