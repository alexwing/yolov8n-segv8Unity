from ultralytics import YOLO

model = YOLO("yolov8n-seg.pt")
# model.export(format="onnx", opset=9)
model.export(format="onnx", opset=9, imgsz=[320, 320])