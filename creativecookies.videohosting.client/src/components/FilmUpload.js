import {
  BlockBlobClient,
  AnonymousCredential,
  newPipeline,
} from "@azure/storage-blob";
import styles from "./FilmUpload.module.css";
import { useState } from "react";
import { Base64 } from "js-base64";
import { Button, Input } from "@mui/material";
import { Search, UploadFile, InsertPhoto } from "@mui/icons-material";

const uploadBlob = async (file, blobName, isVideo) => {
  const account = process.env.REACT_APP_STORAGE_ACCOUNT_NAME;
  const filmContainerName = process.env.REACT_APP_FILMS_CONTAINER_NAME;
  const thumbnailContainerName =
    process.env.REACT_APP_THUMBNAILS_CONTAINER_NAME;
  const apiAddress = process.env.REACT_APP_API_ADDRESS;

  const fetchUrl = isVideo
    ? `https://${apiAddress}/api/SAS/film-upload/${blobName}`
    : `https://${apiAddress}/api/SAS/thumbnail-upload/${blobName}`;
  const response = await fetch(fetchUrl);
  const data = await response.json();
  const sasToken = data.sasToken;

  const blockSize = 100 * 1024 * 1024; // 100MB
  const fileSize = file.size;
  const blockCount = Math.ceil(fileSize / blockSize);
  const blockIds = Array.from({ length: blockCount }, (_, i) => {
    const id = ("0000" + i).slice(-5); // Create a 5-character zero-padded string
    const encoder = new TextEncoder();
    const idBytes = encoder.encode(id);
    return Base64.encode(idBytes);
  });

  const blobURL = `https://${account}.blob.core.windows.net/${
    isVideo ? filmContainerName : thumbnailContainerName
  }/${encodeURIComponent(blobName)}?${sasToken}`;
  debugger;
  const pipeline = newPipeline(new AnonymousCredential());
  const blobClient = new BlockBlobClient(blobURL, pipeline);

  for (let i = 0; i < blockCount; i++) {
    const start = i * blockSize;
    const end = Math.min(start + blockSize, fileSize);
    const chunk = file.slice(start, end);
    const chunkSize = end - start;
    await blobClient.stageBlock(blockIds[i], chunk, chunkSize);
  }

  await blobClient.commitBlockList(blockIds);

  if (isVideo) {
    const length = await getVideoDuration(file);

    const metadata = {
      length: length.toFixed(0), // length in seconds
    };

    await blobClient.setMetadata(metadata);
  }
};

const getVideoDuration = (file) => {
  return new Promise((resolve, reject) => {
    const video = document.createElement("video");
    const url = URL.createObjectURL(file);

    video.preload = "metadata";
    video.src = url;

    video.onloadedmetadata = () => {
      URL.revokeObjectURL(url);
      resolve(video.duration);
    };

    video.onerror = () => {
      URL.revokeObjectURL(url);
      reject(new Error("Error loading video metadata"));
    };
  });
};

const FilmUpload = (props) => {
  const [video, setVideo] = useState();
  const [thumbnail, setThumbnail] = useState();

  const videoChangeHandler = (e) => {
    if (e.target.files) {
      if (e.target.files[0].name.includes(".mp4")) {
        setVideo(e.target.files[0]);
      } else {
        setVideo(undefined);
        alert("Only mp4!");
      }
    }
  };

  const thumbnailChangeHandler = (e) => {
    if (e.target.files) {
      if (e.target.files[0].name.includes(".jpg")) {
        setThumbnail(e.target.files[0]);
      } else {
        setThumbnail(undefined);
        alert("Only jpg!");
      }
    }
  };

  const uploadVideoHandler = () => {
    if (!video) {
      return;
    }

    if (thumbnail) {
      let thumbnailName = `${video.name.slice(
        0,
        video.name.lastIndexOf(".")
      )}.jpg`;
      uploadBlob(thumbnail, thumbnailName, false)
        .then((res) => {
          setThumbnail(undefined);
        })
        .catch((error) => {
          console.log("Error while uploading thumbnail: ", error);
        });
    }

    uploadBlob(video, video.name, true)
      .then((res) => {
        setVideo(undefined);
      })
      .catch((error) => {
        console.log("Error while uploading video: ", error);
      });
  };

  let videoInputDescription = video
    ? `Selected file: ${video.name}`
    : "No file selected";

  let thumbnailInputDescription = thumbnail
    ? `Selected file: ${thumbnail.name}`
    : "No file selected";

  return (
    <div className={styles.container}>
      <div className="row">
        <div className="row">
          <div className="col-6">
            <label
              htmlFor="select-film"
              className={styles["custom-file-upload"]}
            >
              <Search />
              Select mp4 file
            </label>
            <Input id="select-film" type="file" onChange={videoChangeHandler} />
          </div>
          <div className="col-6">
            <span className={styles.description}>{videoInputDescription}</span>
          </div>
        </div>
        <div className="row">
          <div className="col-6">
            <label
              htmlFor="select-thumbnail"
              className={styles["custom-file-upload"]}
            >
              <InsertPhoto />
              Select thumbnail file
            </label>
            <Input
              id="select-thumbnail"
              type="file"
              onChange={thumbnailChangeHandler}
            />
          </div>
          <div className="col-6">
            <span className={styles.description}>
              {thumbnailInputDescription}
            </span>
          </div>
        </div>
        <Button
          variant="contained"
          endIcon={<UploadFile />}
          onClick={uploadVideoHandler}
        >
          Upload
        </Button>
      </div>
    </div>
  );
};

export default FilmUpload;
