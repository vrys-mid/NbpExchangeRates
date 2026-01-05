import { useParams } from "react-router-dom";
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../api/client";


 export const CurrencyDetailsPage = () => {
  const { code } = useParams();

  const { data } = useQuery({
    queryKey: ["history", code],
    queryFn: async () => {
      const res = await apiClient.get(`/rates/${code}/history`);
      return res.data;
    },
  });

  return (
    <div className="bg-white p-6 rounded shadow">
      <h2 className="text-2xl font-bold mb-4">{code}</h2>

      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data}>
          <XAxis dataKey="date" />
          <YAxis />
          <Tooltip />
          <Line type="monotone" dataKey="rate" strokeWidth={2} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};
 