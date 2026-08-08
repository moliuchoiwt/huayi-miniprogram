<template>
    <view>
        <view class="form">
            <up-form labelPosition="top" label-width="700rpx">
                <up-form-item label="照片列表" required>
                    <view style="">
                        <up-upload :fileList="fileList1" @afterRead="afterRead" @delete="deletePic" :name="1" multiple
                            :maxCount="10" width="200rpx" height="200rpx" previewFullImage>
                            <view class="uploadBox">
                                <image src="/static/icon16.png"></image>
                                <view class="text">上传照片</view>
                            </view>
                        </up-upload>
                    </view>
                </up-form-item>
                <up-form-item label="任务金额（元）" required>
                    <view style="width: 100%;">
                        <up-input v-model="query.price" type="number">
							<template #suffix>
								元 
							</template>
						</up-input>
                    </view>
                </up-form-item>
                <up-form-item label="收货方式" required>
                    <up-radio-group v-model="query.receivingType">
                        <up-radio label="物流" :name="0"></up-radio>
                        <up-radio label="自提" :name="1"></up-radio>
                    </up-radio-group>
                </up-form-item>
                <up-form-item label="收货时间" required>
                    <view style="width: 100%;" @click="timePickerShow = true">
                        <up-input v-model="query.receivingTime" readonly placeholder="请选择收货时间">
                            <template #suffix>
                                <up-icon name="arrow-right" color="#969696" size="20"></up-icon>
                            </template>
                        </up-input>
                    </view>
                </up-form-item>
                <up-form-item label="相关需求" required>
                    <up-textarea v-model="query.relatedDemand" placeholder="请输入相关需求"></up-textarea>
                </up-form-item>
                <up-form-item label="备注" required>
                    <up-textarea v-model="query.remarks" placeholder="请输入备注"></up-textarea>
                </up-form-item>
                <up-form-item label="收货地址" required>
                    <picker mode="region" @change="bindTimeChange" style="width: 100%;">
                        <view
                            style="display: flex;align-items: center;justify-content: space-between;width: 100%;padding: 12rpx 0;">
                            <up-input placeholder="请选择省市区" type="text" readonly v-model="addressName" border="none" />
                            <view><up-icon name="arrow-right" color="#969696" size="20"></up-icon></view>
                        </view>
                    </picker>
                </up-form-item>
                <up-form-item label="详细地址" required>
                    <up-textarea v-model="query.address" placeholder="请选择详细地址具体到街道门牌号"></up-textarea>
                </up-form-item>
            </up-form>
            <view style="height: 150rpx;"></view>
        </view>
        <up-datetime-picker :show="timePickerShow" :minDate="Number(new Date()) + 1000" @confirm="selectDate"
            placeholder="请选择收货时间" @cancel="timePickerShow = false" @close="timePickerShow = false"
            closeOnClickOverlay></up-datetime-picker>
        <view class="btns">
            <view class="btn" @click="$u.throttle(submit, 1000)">确认发布</view>
        </view>
    </view>
</template>

<script>
export default {
    data() {
        return {
            timePickerShow: false,
            timePickerData: '',
            query: {
                goodsImgs: [],
                remarks: '',
                relatedDemand: '',
                price: 1,
                receivingType: 0,
                receivingTime: '',
                address: '',
                province: '',
                city: '',
                area: '',
            },
            minPrice: 1,
            fileList1: [],
        }
    },
    computed: {
        addressName() {
            let item = this.query
            if (!item.province) return ''
            return item.province + ' ' + item.city + ' ' + item.area
        },
    },
    async onLoad() {
        await this.storeUser.updateUser()
        this.query.province = this.storeUser.user.province
        this.query.city = this.storeUser.user.city
        this.query.area = this.storeUser.user.area
        this.query.address = this.storeUser.user.address
        // 获取配置
        this.request('Other/GetConfig').then(res => {
            this.minPrice = res.data.orderMinPrice
			this.query.price =  res.data.orderMinPrice
        })
    },
    methods: {
        selectDate(e) {
            this.query.receivingTime = this.formatTimestamp(e.value) + ':00'
            this.timePickerShow = false
        },
        chooseAddress() {
            return new Promise((resolve, reject) => {
                uni.chooseAddress({
                    success: (res) => {
                        resolve(res)
                    },
                    fail: (err) => {
                        reject(err)
                    }
                })
            })
        },
        bindTimeChange(e) {
            this.query.province = e.detail.value[0]
            this.query.city = e.detail.value[1]
            this.query.area = e.detail.value[2]
        },
        // 新增图片
        async afterRead(event) {
            let lists = [].concat(event.file)
            let fileListLen = this[`fileList${event.name}`].length
            lists.map((item) => {
                this[`fileList${event.name}`].push({
                    ...item,
                    status: 'uploading',
                    message: '上传中'
                })
            })
            for (let i = 0; i < lists.length; i++) {
                const result = await this.uploadFile(lists[i].url)
                let item = this[`fileList${event.name}`][fileListLen]
                this[`fileList${event.name}`].splice(fileListLen, 1, Object.assign(item, {
                    status: 'success',
                    message: '',
                    url: result
                }))
                switch (event.name) {
                    case 1:
                        this.query.goodsImgs.push(result)
                        break;
                }
                fileListLen++
            }
        },
        // 删除图片
        deletePic(event) {
            this[`fileList${event.name}`].splice(event.index, 1)
            switch (event.name) {
                case 1:
                    this.query.goodsImgs.splice(event.index, 1)
                    break;
            }
        },
        // 提交
        submit() {
            let rules = [
                ['goodsImgs', '请上传商品照片'],
                ['province', '请选择地址'],
                ['address', '请选择详细地址'],
            ]
            for (let i = 0; i < rules.length; i++) {
                let item = rules[i]
                if (!this.query[item[0]]) {
                    this.toast(item[1])
                    return
                }
            }
            let queryTemp = {
                ...this.query,
                goodsImgs: this.query.goodsImgs.join(','),
            }
			if(this.query.price < this.minPrice){
				this.toast(`最低金额不能低于${this.minPrice}元`)
				return
			}
            this.request('Order/CreateTask', queryTemp).then(res2 => {
                this.payment(res2, '/pages2ShopPort/order/index?index=1')
            })
        }, 
        formatTimestamp(timestamp) {
            const date = new Date(timestamp);
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0'); // 月份从0开始
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            return `${year}/${month}/${day} ${hours}:${minutes}`;
        }
    }
}
</script>

<style lang="scss" scoped>
.btns {
    z-index: 99;
    padding: 15rpx 32rpx;
    width: 100%;
    position: fixed;
    left: 0;
    bottom: 0;
    box-shadow: 0rpx 0rpx 16rpx 1rpx rgba(12, 106, 106, 0.1);
    background: #FFFFFF;

    .btn {
        width: 100%;
        text-align: center;
        padding: 20rpx 0;
        font-size: 28rpx;
        color: #FFFFFF;
        background: #8C4FFF;
        border-radius: 16rpx;
    }
}

.form {
    background: white;
    padding: 0 32rpx;
    width: 686rpx;
    margin: 32rpx;
    border-radius: 16rpx;
}

.item {
    width: 100%;
    display: flex;
    justify-content: space-between;
    padding-right: 32rpx;
}
</style>