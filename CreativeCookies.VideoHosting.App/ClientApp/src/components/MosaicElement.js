import { Link, NavLink } from "react-router-dom";
import styles from "./MosaicElement.module.css";
import { useEffect, useState } from "react";
import { BlobServiceClient } from "@azure/storage-blob";

// HACK: 1 add thumbnails SAS endpoint
// 2 insert image inside of a NavLink
// 3 TODO: find a way to retrieve and see length of a video in
// the right bottom corner

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
    fetchSasToken(props.thumbnail).then((sasToken) => {
      const blob = fetchBlob(props.thumbnail, sasToken).then((blob) => {
        setBlobImage(URL.createObjectURL(blob));
      });
    });
  }, [props.thumbnail]);

  const filmTitle = props.film.name.slice(0, props.film.name.lastIndexOf("."));

  return (
    <Link to={"/player/" + props.film.name} className={styles.linkImage}>
      <div className={styles.imageContainer}>
        <img src={blobImage} alt="thumbnail" className={styles.thumbnail} />
        <div className={styles.badge}>{props.duration + " s"}</div>
      </div>
      <p className={styles.videoTitle}>{filmTitle}</p>
    </Link>
    // <Link to={"/player/" + props.film.name} style={styles.linkImage}>
    //   <img src={blobImage} alt="thumbnail" />
    //   <p className={styles.videoTitle}>{filmTitle}</p>
    // </Link>
  );
};

export default MosaicElement;
