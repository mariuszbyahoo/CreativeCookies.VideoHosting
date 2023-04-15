import { useNavigate, useParams } from "react-router-dom";
import styles from "./Player.module.css";
import "plyr-react/plyr.css";
import Plyr from "plyr-react";
import { useEffect, useRef, useState } from "react";

const Player = (props) => {
  const [videoUrl, setVideoUrl] = useState("");
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const params = useParams();
  const ref = useRef(null);

  if (params.title === ":title") navigate("/films-list");

  useEffect(() => {
    setLoading(true);
    fetchSasToken().then((response) => {
      let lookup = `https://${process.env.REACT_APP_STORAGE_ACCOUNT_NAME}.blob.core.windows.net/films/${params.title}?${response}`;
      setVideoUrl(
        `https://${process.env.REACT_APP_STORAGE_ACCOUNT_NAME}.blob.core.windows.net/films/${params.title}?${response}`
      );
      setLoading(false);
    });
  }, []);

  async function fetchSasToken() {
    const response = await fetch(
      `https://${process.env.REACT_APP_API_ADDRESS}/api/sas/film/${params.title}`
    );
    const data = await response.json();
    return data.sasToken;
  }

  const videoOptions = undefined;
  const plyrVideo = videoUrl && (
    <Plyr
      ref={ref}
      source={{
        type: "video",
        sources: [
          {
            src: videoUrl,
            provider: "html5",
            type: "video/mp4",
            provider: "html5",
          },
        ],
      }}
      options={videoOptions}
    />
  );

  let content;

  if (loading) {
    content = <p>loading</p>;
  } else {
    content = plyrVideo;
  }

  return <div className={styles.container}>{content}</div>;
};
export default Player;
