import styles from "./Mosaic.module.css";
import MosaicElement from "./MosaicElement";

const Mosaic = (props) => {
  return (
    <div className={styles["mosaic-wrapper"]}>
      {props.filmBlobs.map((blob, index) => (
        <MosaicElement
          film={blob.name}
          thumbnail={blob.thumbnailName}
          duration={blob.length}
          createdOn={blob.createdOn}
          key={index}
        />
      ))}
    </div>
  );
};

export default Mosaic;
