import { useEffect } from "react";

export const usePlaceholder = (message: string) => {
  useEffect(() => {
    console.info(message);
  }, [message]);
};
