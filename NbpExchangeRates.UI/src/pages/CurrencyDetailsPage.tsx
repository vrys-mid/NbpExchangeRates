import { useState, useMemo } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, Legend } from "recharts";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../api/client";
import { fetchRatesByDate } from "../api/ratesApi";

export const CurrencyDetailsPage = () => {
  const { code } = useParams();
  const navigate = useNavigate();

  // Calculate default dates (last 30 days)
  const getDefaultDates = () => {
    const end = new Date();
    const start = new Date();
    start.setDate(start.getDate() - 30);
    return {
      start: start.toISOString().split('T')[0],
      end: end.toISOString().split('T')[0]
    };
  };

  const defaultDates = getDefaultDates();
  const [startDate, setStartDate] = useState(defaultDates.start);
  const [endDate, setEndDate] = useState(defaultDates.end);
  const [compareCode, setCompareCode] = useState<string>("");

  // Fetch available currencies for the dropdown
  const { data: currencies } = useQuery({
      queryKey: ["rates", null], queryFn: () => fetchRatesByDate(null),
  });
  // Fetch main currency data
  const { data: mainData, isLoading: mainLoading } = useQuery({
    queryKey: ["history", code],
    queryFn: async () => {
      const res = await apiClient.get(`/rates/${code}/history`);
      return res.data;
    },
  });

  // Fetch comparison currency data
  const { data: compareData, isLoading: compareLoading } = useQuery({
    queryKey: ["history", compareCode],
    queryFn: async () => {
      if (!compareCode) return null;
      const res = await apiClient.get(`/rates/${compareCode}/history`);
      return res.data;
    },
    enabled: !!compareCode,
  });

  // Merge and filter data based on selected date range
  const chartData = useMemo(() => {
    if (!mainData) return [];
    
    // Create a map of dates to rates for the main currency
    const dataMap = new Map();
    
    mainData.forEach((item: any) => {
      const itemDate = new Date(item.date);
      const start = new Date(startDate);
      const end = new Date(endDate);
      
      if (itemDate >= start && itemDate <= end) {
        dataMap.set(item.date, {
          date: item.date,
          [code!]: item.rate,
        });
      }
    });

    // Add comparison currency data if available
    if (compareData && compareCode) {
      compareData.forEach((item: any) => {
        const itemDate = new Date(item.date);
        const start = new Date(startDate);
        const end = new Date(endDate);
        
        if (itemDate >= start && itemDate <= end) {
          const existing = dataMap.get(item.date) || { date: item.date };
          existing[compareCode] = item.rate;
          dataMap.set(item.date, existing);
        }
      });
    }

    // Convert map to array and sort by date
    return Array.from(dataMap.values()).sort((a, b) => 
      new Date(a.date).getTime() - new Date(b.date).getTime()
    );
  }, [mainData, compareData, startDate, endDate, code, compareCode]);

  const handleReset = () => {
    const defaults = getDefaultDates();
    setStartDate(defaults.start);
    setEndDate(defaults.end);
  };

  const isLoading = mainLoading || (compareCode && compareLoading);

  if (mainLoading) return <p>Loading...</p>;

  return (
    <div className="bg-white p-6 rounded shadow">
      <button
        onClick={() => navigate(-1)}
        className="mb-4 inline-flex items-center gap-2 rounded bg-gray-200 px-4 py-2 text-sm hover:bg-gray-300"
      >
        ← Back
      </button>

              <button
          onClick={handleReset}
          className="rounded text-white px-4 py-2 hover:bg-blue-600"
        >
          Reset to Last 30 Days
        </button>
      
      <h2 className="text-2xl font-bold mb-4">{code} Exchange Rate History</h2>
      
      {/* Date Range Controls */}
      <div className="mb-6 design: grid flex-wrap items-end gap-4 p-4 bg-gray-50 rounded">
        <div className="flex-1 min-w-[200px]">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Start Date
          </label>
          <input
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            max={endDate}
            className="rounded border px-3 py-2"
          />
        </div>
        
        <div className="flex-1 min-w-[200px]">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            End Date
          </label>
          <input
            type="date"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
            min={startDate}
            className="rounded border px-3 py-2"
          />
        </div>

        <div className="flex-1 min-w-[200px]">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Compare with
          </label>
          <select
            value={compareCode}
            onChange={(e) => setCompareCode(e.target.value)}
            className=" rounded border px-3 py-2"
          >
            <option value="">No comparison</option>
            {currencies
              ?.filter(c => c.code !== code)
              .map((c) => (
                <option key={c.code} value={c.code}>
                  {c.code} - {c.currency}
                </option>
              ))}
          </select>
        </div>
        

      </div>

      {/* Chart */}
      {isLoading ? (
        <div className="text-center py-8 text-gray-500">Loading comparison data...</div>
      ) : chartData.length > 0 ? (
        <ResponsiveContainer width="100%" height={400}>
          <LineChart data={chartData}>
            <XAxis 
              dataKey="date" 
              tick={{ fontSize: 12 }}
              angle={-45}
              textAnchor="end"
              height={80}
            />
            <YAxis 
              domain={['auto', 'auto']}
              tick={{ fontSize: 12 }}
            />
            <Tooltip />
            <Legend />
            <Line 
              type="monotone" 
              dataKey={code!}
              name={code}
              stroke="#3b82f6"
              strokeWidth={2}
              dot={false}
            />
            {compareCode && (
              <Line 
                type="monotone" 
                dataKey={compareCode}
                name={compareCode}
                stroke="#ef4444"
                strokeWidth={2}
                dot={false}
              />
            )}
          </LineChart>
        </ResponsiveContainer>
      ) : (
        <div className="text-center py-8 text-gray-500">
          No data available for the selected date range
        </div>
      )}
      
      <div className="mt-4 text-sm text-gray-600">
        Showing {chartData.length} data points from {startDate} to {endDate}
        {compareCode && ` (comparing ${code} with ${compareCode})`}
      </div>
    </div>
  );
};