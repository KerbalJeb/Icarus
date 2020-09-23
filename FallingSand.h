//
// Created by kellm on 2020-09-22.
//

#ifndef SFMLTEST_FALLINGSAND_H
#define SFMLTEST_FALLINGSAND_H

#include <cstdint>
#include <exception>

struct Point;

enum class Materials: uint32_t {
    empty=0x000000FF,
    sand =0xC9AEDFFF,
    solid=0xFFFFFFFF
};

struct Point{
    Materials material=Materials::empty;
    bool updated{};
};

class FallingSand {
private:
    uint32_t* pixel_data;
    Point* points;
    Point edgePoint{Materials::solid, false};
    const unsigned int x_dim;
    const unsigned int y_dim;

    Point& get_point(unsigned int x, unsigned int y){
        if (x<0 || y<0 || y>=y_dim || x>=x_dim){
            throw std::out_of_range("Out of bounds");
        }
        return points[x + y*x_dim];
    }

    uint32_t& get_pixel(unsigned int x, unsigned int y){
        if (x<0 || y<0 || y>=y_dim || x>=x_dim){
            throw std::out_of_range("Out of bounds");
        }
        return pixel_data[x + y*x_dim];
    }

    void update_sand(Point& p, unsigned int x, unsigned int y){
        try {
            Point& p2 = get_point(x, y+1);
            if (p2.material == Materials::empty){
                p2.material = Materials::sand;
                p.material = Materials::empty;
                get_pixel(x, y) = static_cast<uint32_t>(p.material);
                get_pixel(x, y+1) = static_cast<uint32_t>(p2.material);
                return;
            }
        }
        catch (std::out_of_range& e) {
            return;
        }

        try {
            Point& p2 = get_point(x-1, y+1);
            if (p2.material == Materials::empty){
                p2.material = Materials::sand;
                p.material = Materials::empty;
                get_pixel(x, y) = static_cast<uint32_t>(p.material);
                get_pixel(x-1, y+1) = static_cast<uint32_t>(p2.material);
                return;
            }
        }
        catch (std::out_of_range& e) {}

        try {
            Point& p2 = get_point(x+1, y+1);
            if (p2.material == Materials::empty){
                p2.material = Materials::sand;
                p.material = Materials::empty;
                get_pixel(x, y) = static_cast<uint32_t>(p.material);
                get_pixel(x+1, y+1) = static_cast<uint32_t>(p2.material);
                return;
            }
        }
        catch (std::out_of_range& e) {}
    }

public:
    FallingSand(unsigned int x, unsigned int y): x_dim{x}, y_dim{y}{
        pixel_data = new uint32_t [x*y]();
        points = new Point [x*y]();

        for (int i = 0; i < x_dim * y_dim; ++i) {
            if (i%x_dim==0 || i%x_dim==(x_dim-1) || i/y_dim == 0 || i/y_dim == (y_dim-1)){
                points[i].material=Materials::solid;
            } else{
                points[i].material=Materials::empty;
            }
            pixel_data[i] = 0x000000FF;
        }
    }

    void addElement(Materials mat, int x, int y){
        get_point(x, y).material = mat;
    }

    void update(){
        for (int y = y_dim-1; y >= 0; --y) {
            for (int x = 0; x < x_dim; ++x) {
                Point& point = get_point(x, y);
                switch (point.material) {
                    case Materials::empty:
                    case Materials::solid:
                        continue;
                    case Materials::sand:
                        update_sand(point, x, y);
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
