import { useParams } from "react-router-dom";
import styles from "./Player.module.css";
import Plyr from "plyr";
import { useEffect, useState } from "react";

const Player = (props) => {
  const [videoUrl, setVideoUrl] = useState("");

  const params = useParams();

  useEffect(() => {}, []);

  async function fetchSasToken() {
    const response = await fetch(
      `https://localhost:7276/api/Tokens/films/${params.title}`
    );
    const data = await response.json();
    return data.sasToken;
  }
  const plyrProps = {
    source: undefined,
  };

  return (
    <div className={styles.container}>
      <h4>Here will be film, playing a film with title of: {params.title}</h4>
      <div id="player"></div>
    </div>
  );
};

export default Player;
