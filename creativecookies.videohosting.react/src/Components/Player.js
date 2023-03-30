import { useParams } from "react-router-dom";
import styles from "./Player.module.css";
import "plyr-react/plyr.css";
import Plyr from "plyr-react";
import { useEffect, useRef, useState } from "react";

const Player = (props) => {
  const [videoUrl, setVideoUrl] = useState("");
  const [loading, setLoading] = useState(true);
  const params = useParams();
  const ref = useRef(null);

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
      `https://localhost:7276/api/Tokens/film/${params.title}`
    );
    const data = await response.json();
    return data.sasToken;
  }
  const plyrProps = {
    type: "video",
    sources: {
      src: "https://mytubestoragecool.blob.core.windows.net/films/Konrad%20Berkowicz,%20czyli%20podr%C3%B3bka.mp4?sv=2021-12-02&st=2023-03-30T17%3A29%3A13Z&se=2023-03-30T17%3A35%3A13Z&sr=b&sp=r&sig=uU4EQunrvZUvmzUoRFFC%2BPbCvfSRmn42wfWKwFYi65U%3D",
      type: "video/mp4",
      size: "1080",
      provider: "html5",
    },
  };

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
