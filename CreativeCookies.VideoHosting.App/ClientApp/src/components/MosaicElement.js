import { Link, NavLink } from "react-router-dom";
import styles from "./MosaicElement.module.css";
import { useEffect, useState } from "react";
import { BlobServiceClient } from "@azure/storage-blob";

const fetchSasToken = async (title) => {
  const response = await fetch(
    `https://${process.env.REACT_APP_API_ADDRESS}/api/sas/thumbnail/${title}`
  );
  const data = await response.json();
  return data.sasToken;
};

const fetchBlob = async (blobNameArray, sasToken) => {
  const blobServiceClient = new BlobServiceClient(
    `https://${process.env.REACT_APP_STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
  );

  const containerClient = blobServiceClient.getContainerClient(
    process.env.REACT_APP_CONTAINER_NAME
  );
  const blockBlobClient = containerClient.getBlockBlobClient(blobNameArray[0]);
  try {
    const response = await blockBlobClient.download(0);
    const imageBlob = await response.blobBody;
    return imageBlob;
  } catch (error) {
    console.error("Error fetching image blob:", error);
    return null;
  }
};

const MosaicElement = (props) => {
  const [blobImage, setBlobImage] = useState(undefined);

  useEffect(() => {
    if (
      props.thumbnail == null ||
      props.thumbnail == undefined ||
      (props.thumbnail && props.thumbnail.length == 0)
    ) {
      setBlobImage(`${process.env.PUBLIC_URL}/blank_thumbnail.png`); // Set the path to the default image
    } else {
      fetchSasToken(props.thumbnail).then((sasToken) => {
        const blob = fetchBlob(props.thumbnail, sasToken).then((blob) => {
          setBlobImage(URL.createObjectURL(blob));
        });
      });
    }
  }, [props.thumbnail]);

  const filmTitle = props.film.name.slice(0, props.film.name.lastIndexOf("."));

  const videoDurationToString = (seconds) => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const remainingSeconds = seconds % 60;

    // Pad with leading zeros if necessary
    const formattedHours = String(hours).padStart(2, "0");
    const formattedMinutes = String(minutes).padStart(2, "0");
    const formattedSeconds = String(remainingSeconds).padStart(2, "0");

    let timeSpan = `${formattedMinutes}:${formattedSeconds}`;
    if (formattedHours > 0) {
      timeSpan = `${formattedHours}:${timeSpan}`;
    }
    return timeSpan;
  };

  return (
    <div className={styles.boxShadowCard}>
      <Link to={"/player/" + props.film.name} className={styles.linkImage}>
        <div className={styles.imageContainer}>
          <img src={blobImage} alt="thumbnail" className={styles.thumbnail} />
          <div className={styles.badge}>
            {videoDurationToString(props.duration)}
          </div>
        </div>
        <p className={styles.videoTitle}>{filmTitle}</p>
      </Link>
    </div>
  );
};

export default MosaicElement;
