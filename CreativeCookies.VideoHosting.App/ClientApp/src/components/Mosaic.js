import styles from "./Mosaic.module.css";
import MosaicElement from "./MosaicElement";

const Mosaic = (props) => {
  return (
    <div className={styles["mosaic-wrapper"]}>
      {props.filmBlobs.map((blob, index) => (
        <MosaicElement
          film={blob}
          thumbnail={props.thumbnailBlobs.filter((b) =>
            b.includes(blob.name.slice(0, blob.name.length - 4))
          )}
          duration={blob.metadata.length}
          key={index}
        />
      ))}
    </div>
  );
};

export default Mosaic;
