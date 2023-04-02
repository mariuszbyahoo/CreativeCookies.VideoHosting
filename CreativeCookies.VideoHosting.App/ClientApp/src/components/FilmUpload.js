import { BlockBlobClient, AnonymousCredential } from "@azure/storage-blob";
import styles from "./FilmUpload.module.css";
import { useState } from "react";

const uploadLargeFile = async (file) => {
  const blobName = file.name;

  const response = await fetch(
    `https://${process.env.REACT_APP_API_ADDRESS}/api/SAS/container`
  );
  const data = await response.json();
  const sasToken = data.sasToken;

  const blockSize = 4 * 1024 * 1024; // 4 MB
  const fileSize = file.size;
  const blockCount = Math.ceil(fileSize / blockSize);
  const blockIds = Array.from({ length: blockCount }, (_, i) =>
    btoa(
      String.fromCharCode.apply(
        null,
        new Uint8Array(new Uint32Array([i]).buffer)
      )
    ).replace(/=/g, "")
  );

  const blobClient = new BlockBlobClient(
    `https://${process.env.REACT_APP_STORAGE_ACCOUNT_NAME}.blob.core.windows.net/${process.env.REACT_APP_CONTAINER_NAME}/${blobName}${sasToken}`,
    new AnonymousCredential()
  );

  for (let i = 0; i < blockCount; i++) {
    const start = i * blockSize;
    const end = Math.min(start + blockSize, fileSize);
    const chunk = file.slice(start, end);
    // HACK: TODO, ADD Controller and action for SAS token with uploading permission.
    await blobClient.stageBlock(blockIds[i], chunk);
  }

  await blobClient.commitBlockList(blockIds);
};

const FilmUpload = (props) => {
  const [file, setFile] = useState();

  const description = `https://${process.env.REACT_APP_STORAGE_ACCOUNT_NAME}.blob.core.windows.net/${process.env.REACT_APP_CONTAINER_NAME}/`;

  const fileChangeHandler = (e) => {
    if (e.target.files) {
      setFile(e.target.files[0]);
    }
  };

  const fileUploadHandler = () => {
    if (!file) {
      return;
    }

    uploadLargeFile(file);
  };

  return (
    <div className={styles.container}>
      Upload a file below to add it into films container into {description}
      <br />
      <input
        type="file"
        placeholder="Select film to upload"
        onChange={fileChangeHandler}
      />
      <br />
      <button type="submit" onClick={fileUploadHandler}>
        Upload
      </button>
      <br />
      <br />
      {file && <p>Title of the uploaded film: {file.name}</p>}
    </div>
  );
};

export default FilmUpload;
