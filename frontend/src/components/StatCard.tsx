import "./StatCard.css";

type StatCardProps = {
  title: string;
  value: number | string;
  description: string;
};

const StatCard = ({ title, value, description }: StatCardProps) => {
  return (
    <article className="stat-card card">
      <p className="stat-card-title">{title}</p>
      <p className="stat-card-value">{value}</p>
      <p className="stat-card-description">{description}</p>
    </article>
  );
};

export default StatCard;
