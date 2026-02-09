type PlaceholderCardProps = {
  title: string;
  description: string;
};

const PlaceholderCard = ({ title, description }: PlaceholderCardProps) => {
  return (
    <div>
      <h3>{title}</h3>
      <p>{description}</p>
    </div>
  );
};

export default PlaceholderCard;
