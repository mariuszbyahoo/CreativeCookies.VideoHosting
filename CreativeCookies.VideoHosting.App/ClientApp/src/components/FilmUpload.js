import { BlockBlobClient, AnonymousCredential } from "@azure/storage-blob";

const uploadLargeFile = async (file) => {
  const blobName = "Freshly uploaded film.mp4";

  // Fetch the SAS token from your API
  const response = await fetch(
    `${process.env.REACT_APP_API_ADDRESS}/api/SAS/container`
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
    await blobClient.stageBlock(blockIds[i], chunk);
  }

  await blobClient.commitBlockList(blockIds);
};
const FilmUpload = (props) => {};

export default FilmUpload;
