import { useParams } from "react-router-dom";
import styles from "./Player.module.css";

const Player = (props) => {
  const params = useParams();

  return (
    <div className={styles.container}>
      <h4>Here will be film, playing a film with title of: {params.name}</h4>
    </div>
  );
};

export default Player;
