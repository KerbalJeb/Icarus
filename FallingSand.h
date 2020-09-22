//
// Created by kellm on 2020-09-22.
//

#ifndef SFMLTEST_FALLINGSAND_H
#define SFMLTEST_FALLINGSAND_H

#include <cstdint>

struct Point;

enum class Materials{
    empty,
    sand,
    solid
};

struct Point{
    Materials material=Materials::empty;
    Materials newMaterial=Materials::empty;
};

class FallingSand {
private:
    uint32_t* pixel_data;
    Point* points;
    const unsigned int x_dim;
    const unsigned int y_dim;
public:
    FallingSand(unsigned int x, unsigned int y): x_dim{x}, y_dim{y}{
        pixel_data = new uint32_t [x*y]();
        points = new Point [x*y]();

        for (int i = 0; i < x_dim * y_dim; ++i) {
            if (i%x_dim==0 || i%x_dim==(x_dim-1) || i/y_dim == 0 || i/y_dim == (y_dim-1)){
                points[i].material=Materials::solid;
                points[i].newMaterial=Materials::solid;
            } else{
                points[i].material   =Materials::empty;
                points[i].newMaterial=Materials::empty;
            }
            pixel_data[i] = 0x000000FF;
        }
    }

    void addElement(Materials mat, int x, int y){
        points[y*x_dim + x].material = mat;
    }

    void update(){
        for (int x = 0; x < x_dim; ++x) {
            for (int y = 0; y < y_dim; ++y) {
                Point& point = points[x + y*x_dim];
                if (point.material==Materials::empty || point.material==Materials::solid){
                    continue;
                }

                if (point.material == Materials::sand){
                    /* Check below*/
                    if (y+1<y_dim){
                        Point& below_cell = points[x + (y+1)*x_dim];
                        if (below_cell.material == Materials::empty){
                            below_cell.newMaterial = Materials::sand;
                            point.newMaterial = Materials::empty;
                        }
                        else{
                            /* Check Left Below*/
                            if (x-1>0 && y+1<y_dim){
                                Point& left_cell = points[x-1 + (y+1)*x_dim];
                                if (left_cell.material == Materials::empty){
                                    left_cell.newMaterial = Materials::sand;
                                    point.newMaterial = Materials::empty;
                                }
                                continue;
                            }

                            /* Check Right */
                            if (x+1<x_dim & y+1<y_dim){
                                Point& right_cell = points[x+1 + (y+1)*x_dim];
                                if (right_cell.material == Materials::empty){
                                    right_cell.newMaterial = Materials::sand;
                                    point.newMaterial = Materials::empty;
                                }
                                continue;
                            }
                        }
                    }
                }
            }
        }

        for (int x = 0; x < x_dim; ++x) {
            for (int y = 0; y < y_dim; ++y) {
                Point& p = points[y*x_dim+x];
                p.material = p.newMaterial;
                uint32_t & color = pixel_data[x + y*x_dim];

                switch (p.material  ) {
                    case Materials::empty:
                        color = 0x000000FF;
                        break;
                    case Materials::sand:
                        color = 0xEDC9AFFF;
                        break;
                    case Materials::solid:
                        color = 0x808080FF;
                        break;
                }
            }
        }
    }

    uint8_t* getPixelData(){
        return static_cast<uint8_t*>(static_cast<void*>(pixel_data));
    }

    ~FallingSand(){
        delete [] pixel_data;
        delete [] points;
    }
};


#endif //SFMLTEST_FALLINGSAND_H
