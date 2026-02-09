import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { apiClient } from "../services/apiClient";
import type { ChangeRequest } from "../types/change";

const ChangeListPage = () => {
  const [changes, setChanges] = useState<ChangeRequest[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    apiClient
      .getChanges()
      .then((data) => setChanges(data))
      .catch((err: Error) => setError(err.message));
  }, []);

  return (
    <section>
      <h2>Change List</h2>
      <Link to="/changes/new">New Change</Link>
      {error ? <p>{error}</p> : null}
      {changes.length > 0 ? (
        <ul>
          {changes.map((change) => (
            <li key={change.id}>
              <Link to={`/changes/${change.id}`}>
                {change.title} ({change.status})
              </Link>
            </li>
          ))}
        </ul>
      ) : (
        <p>Loading changes...</p>
      )}
    </section>
  );
};

export default ChangeListPage;
