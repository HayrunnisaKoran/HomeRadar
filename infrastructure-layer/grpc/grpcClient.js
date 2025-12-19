// infrastructure-layer/grpc/grpcClient.js
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const path = require('path');

const PROTO_PATH = path.join(__dirname, 'protos/prediction.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH);
const predictionProto = grpc.loadPackageDefinition(packageDefinition).smartvalue;

class GrpcClient {
  constructor() {
    this.client = new predictionProto.PredictionService(
      'localhost:50051',
      grpc.credentials.createInsecure()
    );
  }

  async predict(propertyData) {
    return new Promise((resolve, reject) => {
      this.client.Predict(propertyData, (error, response) => {
        if (error) {
          console.error('gRPC Error:', error);
          reject(error);
        } else {
          resolve(response);
        }
      });
    });
  }
}

module.exports = new GrpcClient();